
import { LogInfo } from "../models/AutomationDetailResponse";
import icons from '../assets/icons/bootstrap-icons.svg';
import { Accordion } from "react-bootstrap";

interface Props 
{
    logData : LogInfo;
    index : number
    traceIndex : number
}

function LogEntry(props : Props)
{
    var iconId : string;
    var iconColor : string = 'white';

    const log = props.logData;
    //const idSuffix = props.traceIndex + "-" + props.index;
    switch(log.logLevel)
    {
        case 'Trace':
            iconId = "#bug";
            break;
        case 'Debug':
            iconId = "#bug-fill";
            break;
        case 'Info':
            iconId = "#info-circle";
            break;
        case 'Warn':
            iconId = "#exclamation-triangle";
            iconColor = "yellow";
            break;
        case "Error":
            iconId = "#exclamation-octagon";
            iconColor = "red";
            break;
        case 'Fatal':
            iconId = "#exclamation-octagon-fill";
            iconColor = "red";
            break;
        default:
            iconId = "#question";
            iconColor = "yellow";
            break;
    }    

    return (<>
        <Accordion.Item eventKey={props.index.toString()}>
            <Accordion.Header>
                <svg height={32} width={32} fill={iconColor} fillOpacity={.5} >
                    <use href={icons + iconId} height={32} width={32} />
                </svg>
                &nbsp; {log.message}
            </Accordion.Header>
            <Accordion.Body className="row">
                <div className="col-6">
                        <label className="control-label">Properties:</label>
                        <textarea disabled={true} className="form-control" cols={40} rows={3} defaultValue={JSON.stringify(log.properties,null, 2)} />
                    </div>
                    <div className="col-6">
                        <label className="control-label">Scope:</label>
                        <textarea disabled={true} className="form-control" cols={40} rows={3} defaultValue={JSON.stringify(log.scopes,null, 2)} />
                    </div>
                </Accordion.Body>
        </Accordion.Item>
    </>)
}

export default LogEntry;