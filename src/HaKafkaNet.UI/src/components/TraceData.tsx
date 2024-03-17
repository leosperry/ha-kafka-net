import { Accordion } from "react-bootstrap";
import { TraceDataResponse } from "../models/AutomationDetailResponse";
import LogEntry from "./LogEntry";

interface Props {
    trace : TraceDataResponse;
    index : number
}

function TraceData(props : Props) {

    return (
        <Accordion.Item eventKey={props.index.toString()}>
            <Accordion.Header>
                <div className="row">
                    <div className="col-5">Time: {new Date(props.trace.event.eventTime).toLocaleString()}</div>
                    <div className="col-5">Type: {props.trace.event.eventType}</div>
                </div>
            </Accordion.Header>
            <Accordion.Body>
                {props.trace.event.stateChange ? <><div>State Change</div>
                    <textarea disabled={true} cols={100} rows={10} defaultValue={JSON.stringify(props.trace.event.stateChange, null, 2)} /></>
                    : <></>
                }
                {props.trace.event.exception && <><div>Exception</div>
                    <textarea disabled={true} cols={100} rows={10} defaultValue={JSON.stringify(props.trace.event.exception, null, 2)} />
                </>}
                <div>Logs</div>
                    <Accordion defaultActiveKey={[]} alwaysOpen>
                        {props.trace.logs.map((log, logIndex) => 
                            <LogEntry key={"log" + logIndex} logData={log} index={logIndex} traceIndex={props.index}/>
                        )}
                    </Accordion>
            </Accordion.Body>
        </Accordion.Item>
    )
}

export default TraceData;