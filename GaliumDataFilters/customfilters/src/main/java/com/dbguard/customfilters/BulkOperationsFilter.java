package com.dbguard.customfilters;

import java.io.IOException;
import java.util.ArrayList;
import java.util.Deque;
import java.util.HashSet;
import java.util.LinkedHashSet;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.stream.Collectors;

import org.apache.logging.log4j.Logger;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.JsonMappingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.galliumdata.server.adapters.Variables;
import com.galliumdata.server.handler.GenericPacket;
import com.galliumdata.server.handler.mssql.MSSQLPacket;
import com.galliumdata.server.handler.mssql.SQLBatchPacket;
import com.galliumdata.server.handler.mssql.tokens.TokenDone;
import com.galliumdata.server.logic.FilterResult;
import com.galliumdata.server.logic.RequestFilter;
import com.galliumdata.server.logic.ResponseFilter;
import com.galliumdata.server.repository.FilterUse;

import net.sf.jsqlparser.parser.CCJSqlParserUtil;
import net.sf.jsqlparser.statement.*;
import net.sf.jsqlparser.statement.delete.Delete;
import net.sf.jsqlparser.statement.insert.Insert;
import net.sf.jsqlparser.statement.select.Select;
import net.sf.jsqlparser.statement.update.Update;
import net.sf.jsqlparser.util.TablesNamesFinder;

public class BulkOperationsFilter implements ResponseFilter, RequestFilter {

    private Map<String, Integer> _limitsMap;

    @Override
    public void configure(FilterUse filterUse) {
        var limits = (String)filterUse.getParameters().get("Limits");
        ObjectMapper mapper = new ObjectMapper();

        List<BulkOperationsLimits> list = null;
        try {
            list = mapper.readValue(
                    limits,
                    new TypeReference<List<BulkOperationsLimits>>() {}
            );
        } catch (JsonMappingException e) {
            e.printStackTrace();
        } catch (JsonProcessingException e) {
            e.printStackTrace();
        }

        _limitsMap = list.stream()
                        .collect(Collectors.toMap(
                                BulkOperationsLimits::getTableName,
                                BulkOperationsLimits::getThreshold
                        ));
    }

    @Override
    public String getName() {
        return "BulkOperationsFilter";
    }

    @Override
    public FilterResult filterRequest(Variables variables) {
        Variables connectionContext = (Variables)variables.get("connectionContext");
        SQLBatchPacket pkt = (SQLBatchPacket)variables.get("packet");
        Logger logger = (Logger)variables.get("log");

        var sql = pkt.getSql();

        logger.info("SQL=[" + sql + "]");
        try {
            Deque<List<String>> affectedTables = getAffectedTables(sql);

            connectionContext.put("affectedTables", affectedTables);
        } catch (Exception e) {
            e.printStackTrace();
        }

        return new FilterResult();
    }

    @Override
    public FilterResult filterResponse(Variables variables) {
        Variables connectionContext = (Variables)variables.get("connectionContext");
        GenericPacket pkt = (GenericPacket)variables.get("packet");
        Logger logger = (Logger)variables.get("log");

        if ("Done".equals(pkt.getPacketType())) {

            TokenDone doneToken = (TokenDone)pkt;

            if (!doneToken.isDoneCount()) {
                return new FilterResult();
            }
            long rowCount = doneToken.getRowCount();

            Deque<List<String>> affectedTables = (Deque<List<String>>) connectionContext.get("affectedTables");

            if (affectedTables == null ||
                affectedTables.isEmpty()) {

                return new FilterResult();
            }

            List<String> tables =
                affectedTables.pollFirst();

            List<String> tablesToAlert = new ArrayList<String>();

            for (String tableName : tables) {
                Integer tableThreshold = _limitsMap.get(tableName);
                if(tableThreshold != null){
                    if(rowCount > tableThreshold){
                        tablesToAlert.add(tableName);
                    }
                }
            }

            if(tablesToAlert.size() > 0){

                var alertClient = new AlertServiceClient();
                String username = (String) connectionContext.get("username");
                String ip = (String) connectionContext.get("ip");
                try {
                    alertClient.sendBulkOperationAlert(tablesToAlert, rowCount, username, ip);
                } catch (IOException e) {
                    e.printStackTrace();
                }
            }

            if (doneToken.isDoneFinal()) {
                connectionContext.remove("affectedTables");
            }
        }

        return new FilterResult();
    }

    @Override
    public String[] getPacketTypes() {
        String[] types = { "SQLBatch", "Done"};
        return types;
    }

    public static Deque<List<String>> getAffectedTables(String sql)
            throws Exception {

        Statements statements = CCJSqlParserUtil.parseStatements(sql, parser -> parser.withSquareBracketQuotation(true));

         Deque<List<String>> tables = new LinkedList<>();

        TablesNamesFinder tablesNamesFinder = new TablesNamesFinder();

        for (Statement statement : statements.getStatements()) {

            if (statement instanceof Update update) {

                var tableName = normalizeTableName(update.getTable().getName());
                tables.add(List.of(tableName));
            }

            else if (statement instanceof Delete delete) {

                var tableName = normalizeTableName(delete.getTable().getName());
                tables.add(List.of(tableName));
            }

            else if (statement instanceof Insert insert) {

                var tableName = normalizeTableName(insert.getTable().getName());
                tables.add(List.of(tableName));
            }

            else if (statement instanceof Select) {
                List<String> selectTables = tablesNamesFinder.getTableList(statement)
                .stream()
                .map(BulkOperationsFilter::normalizeTableName)
                .collect(Collectors.toList());

                tables.add(selectTables);
            }
        }

        return tables;
    }

    public static boolean compareTableNames(String table1, String table2) {
        String normalized = normalizeTableName(table2);

        return table1.equalsIgnoreCase(normalized);
    }

    /**
     * Removes square brackets and trims spaces.
     */
    private static String normalizeTableName(String tableName) {
        if (tableName == null || tableName.isBlank()) {
            return "";
        }

        // Remove brackets
        String normalized = tableName
                .replace("[", "")
                .replace("]", "")
                .trim();

        // Keep only last part after '.'
        String[] parts = normalized.split("\\.");
        String lastPart = parts[parts.length - 1];

        return lastPart.toLowerCase();
    }

}
