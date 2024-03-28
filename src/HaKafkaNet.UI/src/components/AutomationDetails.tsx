import { useParams } from 'react-router-dom';
import { useEffect, useState } from "react";

import { Api } from "../services/Api";
import { AutomationDetailsResponse } from '../models/AutomationDetailResponse';
import TraceItem from './TraceItem';
import { Accordion } from 'react-bootstrap';

function AutomationDetails() {
  const { key } = useParams();

  const [data, setData] = useState<AutomationDetailsResponse>();
  const [_error, setError] = useState<unknown>();

  useEffect(() => {
    getData();
  }, []);

  async function getData() {
    if (key) {
      try {
        var data = await Api.GetAutomationDetails(key);
        setData(data);
      } catch (error: any) {
        if (error instanceof Error) {
          setError(error.message);
        } else {
          setError("unknown error: " + error.toString());
        }
      }
    }
  }

  function renderStringArray(arry: string[]): string {
    if (arry.length > 0) {
      return arry.reduce((accumulator, currentValue) => accumulator + ", " + currentValue)
    }
    return "";
  }

  return (<>
    {_error && (<h4 className='bg-danger'>{_error.toString()}</h4>)}
    {!data ? (<div>Loading {key}...</div>) : (<>
      <h2>Automation Details</h2>
      <div className='row'>
        <div className='col-2 overflow-hidden'>Name:</div><div className='col-10'>{data.name}</div>
        <div className='col-2 overflow-hidden'>Description:</div><div className='col-10'>{data.description}</div>
        <div className='col-2 overflow-hidden'>Source:</div><div className='col-10'>{data.source}</div>
        <div className='col-2 overflow-hidden'>Type:</div><div className='col-10'>{data.type}</div>
        <div className='col-2 overflow-hidden'>Key Request:</div><div className='col-10'>{data.keyRequest}</div>
        <div className='col-2 overflow-hidden'>Given Key:</div><div className='col-10'>{data.givenKey}</div>
        <div className='col-2 overflow-hidden'>Event Timings:</div><div className='col-10'>{data.eventTimings}</div>
        <div className='col-2 overflow-hidden'>Trigger IDs:</div><div className='col-10'>{data.triggerIds.length > 0 ? renderStringArray(data.triggerIds) : "none"}</div>
        <div className='col-2 overflow-hidden'>Additional IDs:</div><div className='col-10'>{(data.additionalEntities.length > 0) ? renderStringArray(data.additionalEntities) : "none"}</div>
        <div className='col-2 overflow-hidden'>Is Delayable:</div><div className='col-10'>{data.isDelayable.toString()}</div>
      </div>
      <hr />
      <h3>Trace Data</h3>
      {data.traces.length == 0 ? (<h4>No Trace History</h4>) : (<>
        <Accordion defaultActiveKey={[]} alwaysOpen>
          {data.traces.map((t, index) => <TraceItem key={"trace" + index} trace={t} index={index} />)}
        </Accordion>
      </>)}
    </>)}
  </>);
}

export default AutomationDetails;