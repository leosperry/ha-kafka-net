import { AutomationListResponse, SystemInfo } from "../models/SystemInfo";
import { AutomationDetailsResponse, LogInfo } from "../models/AutomationDetailResponse";

class HknApi {

    private readonly baseUrl: string;
    private readonly sysInfoUrl : string;
    private readonly enableUrl : string;
    private readonly autoDetailsUrl : string;
    private readonly autoListUrl : string;
    private readonly errorLogUrl : string;

    public constructor(){
      this.baseUrl = import.meta.env.VITE_BASE_API_URL ?? '';
      this.sysInfoUrl = this.baseUrl + '/api/systeminfo';
      this.enableUrl = this.baseUrl + '/api/automation/enable';
      this.autoDetailsUrl = this.baseUrl + '/api/automation/';
      this.autoListUrl = this.baseUrl + '/api/automations/';
      this.errorLogUrl = this.baseUrl + '/api/errorlog/';
    }

    async GetAutomationDetails(key: string) : Promise<AutomationDetailsResponse> {
      const response = await fetch(this.autoDetailsUrl + key, {
        mode: "cors"
      });
      if (response.status < 200 || response.status >= 400) {
        throw new Error(response.status.toString() + " " + response.statusText);
      }
      const responseData = await response.json();
      const sysInfo = responseData.data as AutomationDetailsResponse;
      return sysInfo;
  }

     async GetSystemInfo() : Promise<SystemInfo> {
        const response = await fetch(this.sysInfoUrl, {
          mode: "cors"
        });
        if (response.status < 200 || response.status >= 400) {
          throw new Error(response.status.toString() + " " + response.statusText);
        }
        const responseData = await response.json();
        const sysInfo = responseData.data as SystemInfo;
        return sysInfo;
      }

      async GetAutomationList() : Promise<AutomationListResponse> {
        const response = await fetch(this.autoListUrl, {
          mode: "cors"
        });
        if (response.status < 200 || response.status >= 400) {
          throw new Error(response.status.toString() + " " + response.statusText);
        }
  
        const responseData = await response.json();
        const sysInfo = responseData.data as AutomationListResponse;
        return sysInfo;     
      }

      async GetErrorLogs() : Promise<LogInfo[]> {
          const response = await fetch(this.errorLogUrl, {
            mode: "cors"
          });
          if (response.status < 200 || response.status >= 400) {
            throw new Error(response.status.toString() + " " + response.statusText);
          }
    
          const responseData = await response.json();
          const errorlogs = responseData.data as LogInfo[];
          return errorlogs;
      }

      async EnableAutomation(key:string, enable: boolean) : Promise<Response> {
        var payload = {
            key: key,
            enable: enable
        };

        const response = await fetch(this.enableUrl, {
            method: 'POST',
            headers:{
                "Content-Type": "application/json"
            },
            body: JSON.stringify(payload)
        });  
        if (response.status < 200 || response.status >= 400) {
          throw new Error(response.status.toString() + " " + response.statusText);
        }
        return response;
      }
}

export const Api = new HknApi();