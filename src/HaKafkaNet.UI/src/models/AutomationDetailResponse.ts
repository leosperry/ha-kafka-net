
export interface AutomationDetailsResponse {
    name : string;
    description: string;
    keyRequest: string;
    givenKey : string;
    eventTimings: string;
    triggerIds : string[];
    additionalEntities : string[];
    type : string;
    source : string;
    isDelayable : boolean;
    lastTriggered : string;
    lastExecuted : string;
    traces : TraceDataResponse[];
}

export interface TraceDataResponse {
    event : TraceEvent;
    logs : LogInfo[]
}

export interface TraceEvent {
    eventTime : string;
    eventType : string;
    automationKey : string;
    stateChange : object;
    exception: object;
}

export interface LogInfo {
    logLevel : string;
    message : string;
    renderedMessage? : string;
    scopes : any;
    properties : any;
    exception : any;
}
