import { useParams } from 'react-router-dom';
import {useEffect, useState } from "react";

import { Api } from "../services/Api";
import { AutomationDetailsResponse } from '../models/AutomationDetailResponse';
import TraceData from './TraceData';
import { Accordion } from 'react-bootstrap';

function AutomationDetails() {
    const {key} = useParams();

    const [data, setData] = useState<AutomationDetailsResponse>();

    useEffect(() => {
        getData();
      }, []);

      async function getData() {
        if (key) {

            var data = await Api.GetAutomationDetails(key);
            setData(data);        
        }
      }

      if (data) {
        console.debug("additional" + data.additionalEntities.length)
      }

    return (<>
        {!data ? (<div>Loading {key}...</div>): (<>
            <h1>Automation Details</h1>
            <div className='row'>
              <div className='col-2'>Name:</div><div className='col-10'>{data.name}</div>
              <div className='col-2'>Description:</div><div className='col-10'>{data.description}</div>
              <div className='col-2'>Source:</div><div className='col-10'>{data.source}</div>
              <div className='col-2'>Type:</div><div className='col-10'>{data.type}</div>
              <div className='col-2'>Key Request:</div><div className='col-10'>{data.keyRequest}</div>
              <div className='col-2'>Given Key:</div><div className='col-10'>{data.givenKey}</div>
              <div className='col-2'>Event Timings:</div><div className='col-10'>{data.eventTimings}</div>
              <div className='col-2'>Trigger IDs:</div><div className='col-10'>{data.triggerIds}</div>
              <div className='col-2'>Additional IDs:</div><div className='col-10'>{(data.additionalEntities.length > 0) ? data.additionalEntities.map(a => a) : "none"}</div>
              <div className='col-2'>Is Delayable:</div><div className='col-10'>{data.isDelayable.toString()}</div>     
            </div>
            <hr />
            <h3>Trace Data</h3>
            <Accordion defaultActiveKey={[]} alwaysOpen>
              {data.traces.map((t, index) => <TraceData key={"trace" + index} trace={t} index={index}/>)}
            </Accordion>
        </>)}
    </>);
}

export default AutomationDetails;