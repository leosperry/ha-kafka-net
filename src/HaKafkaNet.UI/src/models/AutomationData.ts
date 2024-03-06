export interface AutomationData {
    key : string;
    name : string;
    description : string;
    typeName : string;
    source : string;
    enabled : boolean;
    triggerIds : string[];
    additionalEntitiesToTrack : string[];
    lastTriggered : string;
    lastExecuted : string;
}