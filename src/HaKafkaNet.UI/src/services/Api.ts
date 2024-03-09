import { SystemInfo } from "../models/SystemInfo";

class HknApi {
    private readonly baseUrl: string;
    private readonly sysInfoUrl : string;
    private readonly enableUrl : string;

    public constructor(){
      this.baseUrl = import.meta.env.VITE_BASE_URl ?? '';
      this.sysInfoUrl = this.baseUrl + '/api/systeminfo';
      this.enableUrl = this.baseUrl + '/api/automation/enable';
    }


     async GetSystemInfo() : Promise<SystemInfo> {
        const response = await fetch(this.sysInfoUrl,{
          mode: "cors"
        });
        const responseData = await response.json();
        const sysInfo = responseData.data as SystemInfo;
        return sysInfo;
      }

      async EnableAutomation(key:string, enable: boolean) : Promise<Response> {
        var payload = {
            key: key,
            enable: enable
        };

        return fetch(this.enableUrl, {
            method: 'POST',
            headers:{
                "Content-Type": "application/json"
            },
            body: JSON.stringify(payload)
        });  
      }


}

export const Api = new HknApi();