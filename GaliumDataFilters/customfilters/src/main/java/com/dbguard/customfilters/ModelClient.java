package com.dbguard.customfilters;
import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.net.Socket;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.nio.charset.StandardCharsets;

public class ModelClient {

    private static final String HOST = "dbguard";
    private static final int PORT = 8081;

    public ModelResponse sendQuery(String query) throws IOException {

        byte[] queryBytes = query.getBytes(StandardCharsets.UTF_8);

        try (
                Socket socket = new Socket(HOST, PORT);
                DataOutputStream out = new DataOutputStream(socket.getOutputStream());
                DataInputStream in = new DataInputStream(socket.getInputStream())
        ) {

            out.writeInt(queryBytes.length);

            out.write(queryBytes);
            out.flush();

            byte[] response = new byte[5];
            in.readFully(response);

            boolean isInjection = response[0] == 1;

            float confidence = ByteBuffer
                    .wrap(response, 1, 4)
                    .order(ByteOrder.LITTLE_ENDIAN)
                    .getFloat();

            return new ModelResponse(isInjection, confidence);
        }
    }

}