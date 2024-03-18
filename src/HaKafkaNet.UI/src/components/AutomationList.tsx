import { AutomationData } from "../models/AutomationData";
import AutomationListItem from "./AutomationListItem";
import { useState } from "react";

interface AutoListProps {
    automations: AutomationData[];
}

function AutomationList(autodata : AutoListProps) {
    const [searchTxt, setSearchText] = useState('');

    function filter() : AutomationData[] {
        return autodata.automations.filter(a => {
            const lowered = searchTxt.toLowerCase();
            return searchTxt == '' ||
                a.name.toLowerCase().includes(lowered) ||
                a.description.toLowerCase().includes(lowered) ||
                a.triggerIds.filter(t => t.toLowerCase().includes(lowered)).length > 0 ||
                a.additionalEntitiesToTrack.filter(t => t.toLowerCase().includes(lowered)).length > 0
        });
    }

    return (<>
        <h3>Automations</h3>
        <div className="row">
            <div className="col-8">
                <p>Tip: you can trigger your automations manually by setting entity state in <a href="http://homeassistant.local:8123/developer-tools/state" target="_blank">Home Assistant</a></p>
            </div>
            <div className="col-4">
                <label>
                    Filter: &nbsp; <input type="text" onChange={e => setSearchText(e.target.value)}/>
                </label>
            </div>
        </div>

        <div className="row">
            <div className="col-1">Enabled</div>
            <div className="col-3">Name</div>
            <div className="col-5">Description</div>
            <div className="col-3">Source/Type</div>
            <div className="">
                {filter().map((item, _) =>(<AutomationListItem key={item.key} item={item}/>))}
            </div>
        </div>
    </>);
}

export default AutomationList;