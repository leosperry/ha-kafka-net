import { AutomationData } from "./AutomationData";

export interface SystemInfo {
    stateHandlerInitialized : boolean;
    version : string;
    automations : AutomationData[];
}

export interface AutomationDictionary {
    [Key: string] : AutomationData
}