import { AutomationData } from "../models/AutomationData";
import { useEffect, useState } from "react";
import { Api } from "../services/Api";
import * as reactRouterDom from "react-router-dom";
import * as reactBootstrap from "react-bootstrap";

interface Props {
    item: AutomationData;
    index: number;
}

function AutomationListItem(props: Props) {
    const navigate = reactRouterDom.useNavigate();

    const [enabled, setEnabled] = useState<boolean>(props.item.enabled);

    useEffect(() => {
        setEnabled(props.item.enabled);
    }, []);

    function renderStringArray (arry : string[]) : string{
        if (arry.length > 0) {
            return arry.reduce( (accumulator, currentValue) => accumulator + ", " + currentValue)
        }
        return "";
    }

    async function handleCheckboxChange(e: React.ChangeEvent<HTMLInputElement>) {
        var autokey = e.target.getAttribute('data-key')!;
        var checked = e.target.checked;

        var response = await Api.EnableAutomation(autokey, checked);

        if ((response).ok) {
            setEnabled(!enabled)
        }
    }

    const item = props.item;

    return (<>
        <reactBootstrap.Accordion.Item eventKey={props.index.toString()}>
            <reactBootstrap.Accordion.Header>
                <div className="row mb-1">
                    <div className="col-1">
                        <reactBootstrap.Form.Switch onClick={e => e.stopPropagation()} onChange={handleCheckboxChange} checked={enabled} data-key={item.key} />
                    </div>
                    <div className="col-3 overflow-hidden text-wrap" >{item.name}</div>
                    <div className="col-5 overflow-hidden text-wrap">{item.description}</div>
                    <div className="col-3 overflow-hidden text-wrap">
                        <div>{item.source}</div>
                        <div>{item.typeName}</div>
                    </div>
                </div>
            </reactBootstrap.Accordion.Header>
            <reactBootstrap.Accordion.Body>
                <div className="row">
                    <div className="col-4">
                        <a href={"/hakafkanet/automation/" + item.key} onClick={(e) => { navigate('/automation/' + item.key); e.preventDefault() }} ><reactBootstrap.Button>Details</reactBootstrap.Button> </a>
                        <div className="mt-3">Last Triggered: {item.lastTriggered}</div>
                        {item.isDelayable && <div>Last Executed: {item.lastExecuted}</div>}
                        {item.isDelayable && <div>Next Scheduled: {item.nextScheduled}</div>}
                    </div>
                    <div className="col-4">
                        <div>Trigger IDs:{renderStringArray(item.triggerIds)}</div>
                    </div>
                    <div className="col-4">
                        <div>Additional:{renderStringArray(item.additionalEntitiesToTrack)}</div>
                    </div>
                </div>
            </reactBootstrap.Accordion.Body>
        </reactBootstrap.Accordion.Item>
    </>);
}

export default AutomationListItem;