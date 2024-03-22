import { BrowserRouter, Routes, Route } from "react-router-dom";

import HknHeaderFooter from "./components/HknHeaderFooter";
import AutomationList from "./components/AutomationList";
import AutomationDetails from "./components/AutomationDetails";


function App() {
  return (<>
      <BrowserRouter basename="hakafkanet">
        <HknHeaderFooter>
          <Routes>
            <Route index element={<AutomationList />} />
            <Route path="automation/:key" element={<AutomationDetails />}/>
          </Routes>
        </HknHeaderFooter>
      </BrowserRouter>
  </>);
}

export default App;