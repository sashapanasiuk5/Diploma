package com.dbguard.customfilters;

import java.io.DataOutputStream;
import java.io.IOException;
import java.net.Socket;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.nio.charset.StandardCharsets;
import java.util.List;

public class AlertServiceClient {

    private static final String HOST = "dbguard";
    private static final int PORT = 8082;

    public void sendSQLInjectionAlert(String query, float accuracy, String username, String ip) throws IOException {

        try (Socket socket = new Socket(HOST, PORT);
             DataOutputStream out = new DataOutputStream(socket.getOutputStream())) {

            out.writeByte(1);

            writeString(out, query);

            out.write(floatToLittleEndian(accuracy));

            writeString(out, username);
            writeString(out, ip);

            out.flush();
        }
    }

    public void sendBulkOperationAlert(List<String> tables, long rowCount, String username, String ip) throws IOException {
        try (Socket socket = new Socket(HOST, PORT);
             DataOutputStream out = new DataOutputStream(socket.getOutputStream())) {

            out.writeByte(2);

            out.writeByte(tables.size());

            writeString(out, String.join(",", tables));

            out.write(longToLittleEndian(rowCount));

            writeString(out, username);
            writeString(out, ip);

            out.flush();
        }
    }

    private void writeString(DataOutputStream out, String value) throws IOException {
        byte[] bytes = value.getBytes(StandardCharsets.UTF_8);
        out.write(intToLittleEndian(bytes.length));
        out.write(bytes);
    }

    private byte[] intToLittleEndian(int value) {
        return ByteBuffer.allocate(4)
                .order(ByteOrder.LITTLE_ENDIAN)
                .putInt(value)
                .array();
    }

    private byte[] longToLittleEndian(long value) {
        return ByteBuffer.allocate(8)
                .order(ByteOrder.LITTLE_ENDIAN)
                .putLong(value)
                .array();
    }

    private byte[] floatToLittleEndian(float value) {
        return ByteBuffer.allocate(4)
                .order(ByteOrder.LITTLE_ENDIAN)
                .putFloat(value)
                .array();
    }
}