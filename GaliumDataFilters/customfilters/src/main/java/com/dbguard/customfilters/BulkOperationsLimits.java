package com.dbguard.customfilters;

import com.fasterxml.jackson.annotation.JsonProperty;

public class BulkOperationsLimits {
    @JsonProperty("TableName")
    private String tableName;

    @JsonProperty("Threshold")
    private int threshold;


    public BulkOperationsLimits() {
    }

    public String getTableName() {
        return tableName.toLowerCase();
    }

    public void setTableName(String tableName) {
        this.tableName = tableName;
    }

    public int getThreshold() {
        return threshold;
    }

    public void setThreshold(int threshold) {
        this.threshold = threshold;
    }
}
