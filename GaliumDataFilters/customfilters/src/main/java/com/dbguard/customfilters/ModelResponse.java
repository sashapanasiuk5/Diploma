package com.dbguard.customfilters;

public class ModelResponse {
    public boolean isInjection;
    public float confidence;

    ModelResponse(boolean isInjection, float confidence) {
        this.isInjection = isInjection;
        this.confidence = confidence;
    }
}
