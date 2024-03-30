
import { LogInfo } from "../models/AutomationDetailResponse";
import icons from '../assets/icons/bootstrap-icons.svg';
import { Accordion, Overlay, Tooltip } from "react-bootstrap";
import { useRef, useState } from "react";

interface Props {
    logData: LogInfo;
    index: number;
    traceIndex: number;
    showAutomationLink?: undefined | boolean;
}

function LogEntry(props: Props) {
    const [show, setShow] = useState(false);
    const target = useRef(null);

    var iconId: string;
    var iconColor: string = 'white';

    const copyId = `log${props.traceIndex}.${props.index}`;

    const log = props.logData;

    switch (log.logLevel) {
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

    const copyText: React.MouseEventHandler<SVGSVGElement> = (e) => {
        copyToClipboard(log.message);
        //navigator.clipboard.writeText(log.message);
        setShow(true);
        setTimeout(() => {
            setShow(false);
        }, (2000));
        e.preventDefault();
        e.stopPropagation();
    };

    const copyToClipboard = (content: string) => {
        if (window.isSecureContext && navigator.clipboard) {
            navigator.clipboard.writeText(content);
        } else {
            unsecuredCopyToClipboard(content);
        }
    };
    const unsecuredCopyToClipboard = (text: string) => { const textArea = document.createElement("textarea"); textArea.value = text; document.body.appendChild(textArea); textArea.focus(); textArea.select(); try { document.execCommand('copy') } catch (err) { console.error('Unable to copy to clipboard', err) } document.body.removeChild(textArea) };


    return (<>
        <Accordion.Item eventKey={props.index.toString()}>
            <Accordion.Header>
                <div className="row">
                    <div className="col-1">
                        <svg height={32} width={32} fill={iconColor} fillOpacity={.5} >
                            <use href={icons + iconId} height={32} width={32} />
                        </svg>
                    </div>
                    <div className="col-10 overflow-hidden text-wrap">
                        {log.renderedMessage ?? log.message}
                    </div>
                    <div className="col-1">
                        <svg ref={target} onClick={copyText} height={32} width={32} fill="white" fillOpacity={.5} >
                            <use href={icons + "#copy"} height={16} width={16} />
                        </svg>
                        <Overlay target={target.current} show={show} placement="right">
                            {(props) => (<Tooltip id={copyId} {...props}>Log messaged copied to clipboard.</Tooltip>)}
                        </Overlay>
                    </div>
                </div>
            </Accordion.Header>
            <Accordion.Body>
                <div className="row">
                    <div className="col-12">
                        {props.showAutomationLink && log.scopes && log.scopes["automationKey"] && (<>
                            <h4>Go to <a href={"/hakafkanet/automation/" + log.scopes["automationKey"]}>{log.scopes["automationName"] ?? log.scopes["autommationName"]}</a></h4>
                        </>)}
                    </div>
                </div>

                <div className="row">
                    <div className="col-6">
                        <label className="control-label">Properties:</label>
                        <textarea disabled={true} className="form-control" rows={3} defaultValue={JSON.stringify(log.properties, null, 2)} />
                    </div>
                    <div className="col-6">
                        <label className="control-label">Scope:</label>
                        <textarea disabled={true} className="form-control" rows={3} defaultValue={JSON.stringify(log.scopes, null, 2)} />
                    </div>
                </div>

                {log.exception && (<div className="row"><div className="col-12">
                    <label>Exception:  </label>
                    <textarea disabled={true} className="form-control" rows={3} defaultValue={JSON.stringify(log.exception, null, 2)} />
                </div></div>)
                }
            </Accordion.Body>
        </Accordion.Item>
    </>)
}

export default LogEntry;