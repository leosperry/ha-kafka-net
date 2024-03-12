
export interface AutomationDetailsResponse {
    name : string;
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
    latestStateChange : object;
}
