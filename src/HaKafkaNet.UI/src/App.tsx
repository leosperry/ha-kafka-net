import {useEffect, useState } from "react";
import { BrowserRouter, Routes, Route } from "react-router-dom";

import HknHeader from "./components/HknHeader";
import AutomationList from "./components/AutomationList";
import { SystemInfo } from "./models/SystemInfo";
import { Api } from "./services/Api";
import AutomationDetails from "./components/AutomationDetails";


function App() {
  const [data, setData] = useState<SystemInfo>();

  useEffect(() => {
    getData();
  }, []);

  async function getData() {
    var sysInfo = await Api.GetSystemInfo();
    setData(sysInfo);
  }

  return (<>
    {!data ? (<div>Loading ...</div>): (<>
      <BrowserRouter basename="hakafkanet">
        <HknHeader version={data.version} initialized={data.stateHandlerInitialized}/>
        <Routes>
          <Route index element={<AutomationList automations={data.automations} />} />
          <Route path="automation/:key" element={<AutomationDetails />}/>
        </Routes>
      </BrowserRouter>
      <hr />
      <div>
          Send the developer a thank you, report a bug, or request feature via <a href={"mailto:leonard.sperry@live.com?subject=HaKafkaNet Comment V"+ data.version}>email</a> 
          &nbsp; or <a href="https://github.com/leosperry/ha-kafka-net/discussions" target="_blank">start a discussion</a>
      </div>
    </>)}
  </>);
}

export default App;