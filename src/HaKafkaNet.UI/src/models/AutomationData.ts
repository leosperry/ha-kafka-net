export interface AutomationData {
    key : string;
    name : string;
    description : string;
    typeName : string;
    source : string;
    isDelayable : boolean;
    enabled : boolean;
    triggerIds : string[];
    additionalEntitiesToTrack : string[];
    lastTriggered : string;
    lastExecuted? : string;
    nextScheduled? : string;
}