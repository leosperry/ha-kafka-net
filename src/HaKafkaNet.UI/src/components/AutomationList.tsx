import { AutomationData } from "../models/AutomationData";
import AutomationListItem from "./AutomationListItem";

interface AutoListProps {
    automations: AutomationData[];
}

function AutomationList(autodata : AutoListProps) {
    return (<>
        <h3>Automations</h3>
        <div>
        <p>Tip: you can trigger your automations manually by setting entity state in <a href="http://homeassistant.local:8123/developer-tools/state" target="_blank">Home Assistant</a></p>
        </div>

        <div className="row">
            <div className="col-1">Enabled</div>
            <div className="col-3">Name</div>
            <div className="col-5">Description</div>
            <div className="col-3">Source/Type</div>
            <div className="">
                {autodata.automations.map((item, _) =>(<AutomationListItem key={item.key} item={item}/>))}
            </div>
        </div>
    </>);
}

export default AutomationList;