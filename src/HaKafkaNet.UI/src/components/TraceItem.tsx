import { Accordion } from "react-bootstrap";
import { TraceDataResponse } from "../models/AutomationDetailResponse";
import LogEntry from "./LogEntry";
import icons from '../assets/icons/bootstrap-icons.svg';


interface Props {
    trace : TraceDataResponse;
    index : number
}

function TraceItem(props : Props) {
    const errorIconSize : number = 32;

    return (
        <Accordion.Item eventKey={props.index.toString()}>
            <Accordion.Header>
                <div className="row">
                    <div className="col-5">Time: {new Date(props.trace.event.eventTime).toLocaleString()}</div>
                    <div className="col-4">Type: {props.trace.event.eventType}</div>
                    <div className="col-3">
                        <span>Log Count: {props.trace.logs.length}</span>
                        {props.trace.event.exception && <div className="float-end">
                            <svg  height={errorIconSize} width={errorIconSize} fill="red" fillOpacity={.5} >
                                <use href={icons + "#exclamation-diamond"} height={errorIconSize} width={errorIconSize} />
                            </svg>
                        </div>}   
                    </div>
                </div>
            </Accordion.Header>
            <Accordion.Body>
                {props.trace.event.stateChange ? <><div>State Change</div>
                    <textarea disabled={true} cols={100} rows={5} defaultValue={JSON.stringify(props.trace.event.stateChange, null, 2)} /></>
                    : <></>
                }
                {props.trace.event.exception && <><div>Exception</div>
                    <textarea disabled={true} cols={100} rows={5} defaultValue={JSON.stringify(props.trace.event.exception, null, 2)} />
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

export default TraceItem;