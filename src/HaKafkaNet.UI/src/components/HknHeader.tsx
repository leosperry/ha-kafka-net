
interface HeaderProps {
    version: string;
    initialized: boolean;
}

function HknHeader(props : HeaderProps) {
    //const version = "V5.0.1";
    return (<>
        <div className="float-end">
            <p><a href="/kafkaflow" target="_blank">Kafka Flow Admin</a></p>
            <p><a href="https://github.com/leosperry/ha-kafka-net/wiki" target="_blank">Documentation</a></p>
        </div>
        <h1>Ha-Kafka-Net</h1>
        <div>Version:<span>{props.version}</span></div>
        <div>State Handler Initialized: <span>{props.initialized.toString()}</span></div>
        <hr />
    </>);
}

export default HknHeader;