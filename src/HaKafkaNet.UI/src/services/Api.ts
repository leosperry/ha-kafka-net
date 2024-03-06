import { SystemInfo } from "../models/SystemInfo";

class HknApi {
    
     async GetSystemInfo() : Promise<SystemInfo> {
        const response = await fetch('http://localhost:5062/api/systeminfo',{
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

        return fetch('http://localhost:5062/api/automation/enable', {
            method: 'POST',
            headers:{
                "Content-Type": "application/json"
            },
            body: JSON.stringify(payload)
        });  
        
      }


}

export const Api = new HknApi();