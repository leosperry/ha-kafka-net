import { AutomationData } from "../models/AutomationData";
import {useEffect, useState } from "react";
import { Api } from "../services/Api";
import { useNavigate } from "react-router-dom";
import { Accordion, Button, Form } from "react-bootstrap";

interface Props {
    item : AutomationData;
    index : number;
}

function AutomationListItem(props :Props ) {
    const navigate = useNavigate();

    const [enabled, setEnabled] = useState<boolean>(props.item.enabled);

    useEffect(() => {
        setEnabled(props.item.enabled);
      }, []);
    
    async function handleCheckboxChange(e:React.ChangeEvent<HTMLInputElement>) {
        var autokey = e.target.getAttribute('data-key')!;
        var checked = e.target.checked;

        var response = await Api.EnableAutomation(autokey, checked);

        if ((response).ok) {
            setEnabled(!enabled)
         }        
    }

    function detailsClick(url : string ,evt : React.MouseEvent<HTMLButtonElement, MouseEvent>) {
        console.debug("button is " + evt.button)
        if (evt.button === 1) {
            evt.preventDefault();
            window.open(url, "_blank");
        } else if(evt.button == 0){
            navigate(url);
        }
    }

    const item = props.item;

    return (<>
        <Accordion.Item eventKey={props.index.toString()}>
            <Accordion.Header>
                <div className="row">
                    <div className="col-1">
                        <Form.Switch  onClick={e => e.stopPropagation()} onChange={handleCheckboxChange} checked={enabled} data-key={item.key}/>
                    </div>
                    <div className="col-3" >{item.name}</div>
                    <div className="col-5">{item.description}</div>
                    <div className="col-3">
                        <div>{item.source}</div>
                        <div>{item.typeName}</div>
                    </div>
                </div> 
            </Accordion.Header>
            <Accordion.Body>
                <div className="row">
                    <div className="col-4">
                        <Button variant="primary" onClick={e => detailsClick('/automation/' + item.key, e)} onAuxClick={e => detailsClick(window.location +'/automation/' + item.key, e)}>Details</Button>
                        <div className="mt-3">Last Triggered: {item.lastTriggered}</div>
                        {item.isDelayable && <div>Last Executed: {item.lastExecuted}</div>}
                    </div>
                    <div className="col-4">
                        <div>Trigger IDs:{item.triggerIds.map(trigger =>(<p key={trigger}>{trigger}</p>))}</div>
                    </div>
                    <div className="col-4">
                        <div>Additional:{item.additionalEntitiesToTrack.map(trigger =>(<p key={trigger}>{trigger}</p>))}</div> 
                    </div>
                </div>
            </Accordion.Body>
        </Accordion.Item>
    </>);
}

export default AutomationListItem;