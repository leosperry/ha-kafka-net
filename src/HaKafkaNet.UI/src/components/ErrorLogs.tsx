import { useEffect, useState } from "react";
import { Accordion } from "react-bootstrap";
import { Api } from "../services/Api";
import { LogInfo } from "../models/AutomationDetailResponse";
import LogEntry from "./LogEntry";


function ErrorLogs() {
    const [data, setData] = useState<LogInfo[] | null>(null);

    useEffect(() => {
        getData();
    }, []);

    async function getData() {
        var logs = await Api.GetErrorLogs();

        setData(logs);
    }

    return (<>
        <h2>Error Logs</h2>
        {!data ? (<>Loading ...</>) : (<>
            {data.length == 0 ? (<h4>Hooray! No Logs Found</h4>) : <>
                <Accordion defaultActiveKey={[]} alwaysOpen>
                    {data.map((log, logIndex) =>
                        (<LogEntry key={"log" + logIndex} logData={log} index={logIndex} traceIndex={1} showAutomationLink={true} />))}
                </Accordion>
            </>}
        </>)}
    </>);
}

export default ErrorLogs;