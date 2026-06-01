package com.dbguard.customfilters;

import java.io.IOException;
import java.util.regex.Pattern;

import org.apache.logging.log4j.Logger;

import com.galliumdata.server.adapters.Variables;
import com.galliumdata.server.handler.mssql.*;
import com.galliumdata.server.logic.FilterResult;
import com.galliumdata.server.logic.RequestFilter;
import com.galliumdata.server.repository.FilterUse;

public class SQLInjectionFilter implements RequestFilter {

    private ModelClient _client;

    private float _threashold;

    private int _actionType;

    @Override
    public FilterResult filterRequest(Variables variables) {
        Variables connectionContext = (Variables)variables.get("connectionContext");
        MSSQLPacket pkt = (MSSQLPacket)variables.get("packet");
        Logger logger = (Logger)variables.get("log");
        
        SQLBatchPacket queryPacket = (SQLBatchPacket)pkt;

        String sql = queryPacket.getSql();
        ModelResponse response;

        try {
            _client = new ModelClient();
            response = _client.sendQuery(sql);
        } catch (Exception e) {
            logger.info("ML-model is unavailable");
            return new FilterResult();
        }


        if(response.isInjection && response.confidence > this._threashold){

            if(_actionType == 1 || _actionType == 3){
                var alertClient = new AlertServiceClient();
                String username = (String) connectionContext.get("username");
                String ip = (String) connectionContext.get("ip");
                try {
                    alertClient.sendSQLInjectionAlert(sql, response.confidence, username, ip);
                } catch (IOException e) {
                    logger.info("Alert service is unavailable");
                    logger.info("SQL-injection detected: " + sql);
                }
            }

            if(_actionType == 2 || _actionType == 3){

                FilterResult result = new FilterResult();
                result.setSuccess(false);
                result.setErrorCode(50001);
                result.setErrorMessage("DBGuard: SQL Injection detected");
                return result;
            }
        }

        return new FilterResult();
    }

    @Override
    public String[] getPacketTypes() {
        String[] types = { "SQLBatch"};
        return types;
    }

    @Override
    public void configure(FilterUse filterUse) {
        this._actionType = (Integer)filterUse.getParameters().get("Action");

        var threshold = (String)filterUse.getParameters().get("Threshold");

        this._threashold = Float.parseFloat(threshold);
    }

    @Override
    public String getName() {
        return "SQLInjectionFilter";
    }
}
