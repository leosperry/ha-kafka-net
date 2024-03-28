import { useEffect, useState } from "react";
import { Accordion } from "react-bootstrap";
import { Api } from "../services/Api";
import { LogInfo } from "../models/AutomationDetailResponse";
import LogEntry from "./LogEntry";
import { useLocation, useParams } from "react-router-dom";

function ErrorLogs() {
    const [data, setData] = useState<LogInfo[] | null>(null);
    const [displayInfo, setDisplayInfo] = useState<LogPageDisplayInfo>();
    const { logType } = useParams();
    const location = useLocation(); //force page load when switching to/from error/global

    useEffect(() => {
        getData();
    }, [location.key]);

    async function getData() {
        switch (logType) {
            case "error":
                var logs = await Api.GetLogs(logType);
                setData(logs);
                setDisplayInfo({
                    title: "Error Logs",
                    description: getErrorDescription()
                });
                break;
            case "tracker":
                var logs = await Api.GetLogs(logType);
                setData(logs);
                setDisplayInfo({
                    title: "Entity Tracker Logs",
                    description: getTrackerDescription()
                });
                break; 
            case "global":
                var logs = await Api.GetLogs(logType);
                setData(logs);
                setDisplayInfo({
                    title: "Global Logs",
                    description: getGlobalDescription()
                });
                break;
            default:
                setDisplayInfo({
                    title: "Unknown",
                    description: (<></>)
                });
                break;
        }
    }

    const getErrorDescription = () => (<div>All logs from anywhere in the application with a Log Level of Warning or above are displayed here.</div>);

    const getGlobalDescription = () => (<div>
        For logs on this page, at least one of the following is true
        <ul>
            <li>The log was recorded entirely outside the context of an automaiton or the entity tracker, such as startup or other processes that may be running.</li>
            <li>The log was associated with an automation, but but occurred after a trace completed. This could happen if an automaiton creates independent threads with long running processes.</li>
        </ul>
    </div>);

    const getTrackerDescription = () => (<div>All logs from runs of the <a href="https://github.com/leosperry/ha-kafka-net/wiki/System-Monitor#badentitystatediscovered">entity tracker</a> will be shown here.</div>)

    return (<>
        <h2>{displayInfo?.title}</h2>
        {!data ? (<>Loading ...</>) : (<>
            <div className="m-3">
                {displayInfo?.description}
            </div>
            {data.length == 0 ? (<>
                <h4>No Logs Found</h4>
                <div>Make sure you have <a href="https://github.com/leosperry/ha-kafka-net/wiki/Tracing" target="_blank">log capturing</a> configured.</div>
            </>) : <>
               

                <Accordion defaultActiveKey={[]} alwaysOpen>
                    {data.map((log, logIndex) =>
                        (<LogEntry key={"log" + logIndex} logData={log} index={logIndex} traceIndex={1} showAutomationLink={true} />))}
                </Accordion>
            </>}
        </>)}
    </>);
}

export default ErrorLogs;

interface LogPageDisplayInfo {
    title: string;
    description: JSX.Element;
}