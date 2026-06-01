package com.dbguard.customfilters;

import com.galliumdata.server.adapters.Variables;
import com.galliumdata.server.handler.mssql.Login7Packet;
import com.galliumdata.server.handler.mssql.MSSQLPacket;
import com.galliumdata.server.logic.FilterResult;
import com.galliumdata.server.logic.RequestFilter;
import com.galliumdata.server.repository.FilterUse;

public class UsernameLogger implements RequestFilter {

    @Override
    public void configure(FilterUse arg0) {
    }

    @Override
    public String getName() {
        return "UsernameLogger";
    }

    @Override
    public FilterResult filterRequest(Variables variables) {
        Variables connectionContext = (Variables)variables.get("connectionContext");
        MSSQLPacket pkt = (MSSQLPacket)variables.get("packet");

        Login7Packet queryPacket = (Login7Packet)pkt;

        var username = queryPacket.getUsername();

        connectionContext.put("username", username);

        return new FilterResult();
    }

    @Override
    public String[] getPacketTypes() {
        String[] types = { "Login7"};
        return types;
    }

}
