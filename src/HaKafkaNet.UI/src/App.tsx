import { BrowserRouter, Routes, Route } from "react-router-dom";

import HknHeaderFooter from "./components/HknHeaderFooter";
import AutomationList from "./components/AutomationList";
import AutomationDetails from "./components/AutomationDetails";
import ErrorLogs from "./components/ErrorLogs";

function App() {
  return (<>
    <BrowserRouter basename="hakafkanet">
      <HknHeaderFooter>
        <Routes>
          <Route index element={<AutomationList />} />
          <Route path="automation/:key" element={<AutomationDetails />} />
          <Route path="errorlogs" element={<ErrorLogs />} />
        </Routes>
      </HknHeaderFooter>
    </BrowserRouter>
  </>);
}

export default App;