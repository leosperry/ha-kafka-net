import { AutomationData } from "./AutomationData";

export interface SystemInfo {
    stateHandlerInitialized : boolean;
    version : string;
}

export interface AutomationListResponse {
    automations : AutomationData[]
}
