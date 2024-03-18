import { AutomationData } from "../models/AutomationData";
import {useEffect, useState } from "react";
import { Api } from "../services/Api";
import { useNavigate } from "react-router-dom";



interface Props {
    item : AutomationData
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

    function detailsClick(url : string ,evt : React.MouseEvent<HTMLAnchorElement, MouseEvent>) {
        console.debug("button is " + evt.button)
        if (evt.button === 1) {
            evt.preventDefault();
            window.open(url, "_blank");
        } else if(evt.button == 0){
            navigate(url);
        }
    }

    const item = props.item;
    var additionalId = item.key + "info";
    return (<>
        <div className="card">
            <div className="card-header">
                <div className="row">
                    <div className="col-1">
                        <div className="form-check form-switch">
                            <input className="form-check-input" type="checkbox" onChange={handleCheckboxChange} checked={enabled} data-key={item.key} />
                        </div>
                    </div>
                    <div className="col-3" >
                        <a data-bs-toggle="collapse" href={"#" + additionalId}>{item.name}</a>
                    </div>
                    <div className="col-5">{item.description}</div>
                    <div className="col-3">
                        <div>{item.source}</div>
                        <div>{item.typeName}</div>
                    </div>
                </div> 
            </div>
            <div id={additionalId} className="collapse card-body row">
                <div className="col-4">
                    <a role="button" className="btn btn-primary" onClick={e => detailsClick('/automation/' + item.key, e)} onAuxClick={e => detailsClick(window.location +'/automation/' + item.key, e)}>Details</a>
                    <div>Last Triggered: {item.lastTriggered}</div>
                    {item.isDelayable && <div>Last Executed: {item.lastExecuted}</div>}
                </div>
                <div className="col-4">
                    <div>Trigger IDs:{item.triggerIds.map(trigger =>(<p key={trigger}>{trigger}</p>))}</div>
                </div>
                <div className="col-4">
                    <div>Additional:{item.additionalEntitiesToTrack.map(trigger =>(<p key={trigger}>{trigger}</p>))}</div> 
                </div>
                
            </div>
        </div>
    </>);
}

export default AutomationListItem;