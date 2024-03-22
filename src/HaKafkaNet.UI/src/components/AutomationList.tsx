import { Accordion, InputGroup, Form } from "react-bootstrap";
import { AutomationData } from "../models/AutomationData";
import AutomationListItem from "./AutomationListItem";
import { useEffect, useState } from "react";
import icons from '../assets/icons/bootstrap-icons.svg';
import { Api } from "../services/Api";

function AutomationList() {

    const [data, setData] = useState<AutomationData[]>();

    useEffect(() => {
      getData();
    }, []);
  
    async function getData() {
      var sysInfo = await Api.GetAutomationList();
      setData(sysInfo.automations);
    }


    const [searchTxt, setSearchText] = useState('');

    function filter(autodata : AutomationData[]) : AutomationData[] {

        return autodata.filter(a => {
            const lowered = searchTxt.toLowerCase();
            return searchTxt == '' ||
                a.name.toLowerCase().includes(lowered) ||
                a.description.toLowerCase().includes(lowered) ||
                a.source.toLocaleLowerCase().includes(lowered) ||
                a.triggerIds.filter(t => t.toLowerCase().includes(lowered)).length > 0 ||
                a.additionalEntitiesToTrack.filter(t => t.toLowerCase().includes(lowered)).length > 0
        });
    }

    return !data ? (<>Loading ...</>) : (<>
        <h2>Automations</h2>
        <div className="float-start">Tip: You can manually trigger automations <br />by setting entity state in <a href="http://homeassistant.local:8123/developer-tools/state" target="_blank">Home Assistant</a></div>
        <div className="float-end w-50">
            <InputGroup className="mb-3" size="lg">
                <InputGroup.Text>
                <svg height={32} width={32} fill="white" fillOpacity={.9} >
                    <use href={icons + "#filter"}  />
                </svg>
                </InputGroup.Text>
                <Form.Control  type="search" placeholder="start typing to filter" onChange={e => setSearchText(e.target.value)} />
            </InputGroup>
        </div>

        <div className="row automation-list-header fs-4">
            <div className="col-1">Enabled</div>
            <div className="col-3">Name</div>
            <div className="col-5">Description</div>
            <div className="col-3">Source/Type</div>
        </div>
        
        <Accordion defaultActiveKey={[]} alwaysOpen>
            {filter(data).map((item, index) => (<AutomationListItem key={item.key} item={item} index={index}/>))}
        </Accordion>

    </>);
}

export default AutomationList;