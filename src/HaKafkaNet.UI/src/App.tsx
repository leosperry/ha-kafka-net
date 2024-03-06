import {useEffect, useState } from "react";
import HknHeader from "./components/HknHeader";
import AutomationList from "./components/AutomationList";
import { SystemInfo } from "./models/SystemInfo";
import { Api } from "./services/Api";


function App() {
  const [data, setData] = useState<SystemInfo>();

  useEffect(() => {
    getData();
  }, []);

  async function getData() {
    var sysInfo = await Api.GetSystemInfo()
    setData(sysInfo);
  }

  return (<>
    {!data ? (<div>Loading ...</div>): (<>
      <HknHeader version={data.version} initialized={data.stateHandlerInitialized}/>
      <AutomationList automations={data.automations}/>
      <div>
          Send the developer a thank you, report a bug, or request feature via <a href={"mailto:leonard.sperry@live.com?subject=HaKafkaNet Comment V"+ data.version}>email</a> 
          &nbsp; or <a href="https://github.com/leosperry/ha-kafka-net/discussions" target="_blank">start a discussion</a>
      </div>
    </>)}
  </>);
}

export default App;