package com.dbguard.customfilters;
import java.net.Socket;

import com.galliumdata.server.adapters.Variables;
import com.galliumdata.server.handler.mssql.*;
import com.galliumdata.server.logic.ConnectionFilter;
import com.galliumdata.server.logic.FilterResult;
import com.galliumdata.server.repository.FilterUse;

public class ConnectionIPLogger implements ConnectionFilter{

    @Override
    public void configure(FilterUse arg0) {
        
    }

    @Override
    public String getName() {
        return "ConncetionIPLogger";
    }

    @Override
    public FilterResult acceptConnection(Socket socket, Variables context) {
        Variables connectionContext = (Variables)context.get("connectionContext");
        var ip = socket.getInetAddress().getHostAddress();

        connectionContext.put("ip", ip);

        return new FilterResult();
    }

}
