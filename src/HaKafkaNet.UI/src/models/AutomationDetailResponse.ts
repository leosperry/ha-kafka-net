
export interface AutomationDetailsResponse {
    name : string;
    description: string;
    keyRequest: string;
    givenKey : string;
    eventTimings: string;
    mode: string;
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
    stateChange : StateChange;
    exception: object;
}

export interface LogInfo {
    logLevel : string;
    timeStamp? : Date;
    message : string;
    renderedMessage? : string;
    scopes : any;
    properties : any;
    exception : any;
}

export interface StateChange {
    entityId: string;
    old? : EntityState;
    new : EntityState;
}

export interface EntityState{
    state : string;
    entity_id : string;
    last_changed : Date;
    last_updated : Date;
    context: object;
    attributes : object
}
