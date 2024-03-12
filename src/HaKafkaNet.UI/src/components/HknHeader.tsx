import Logo from '../assets/hkn_128.png';
import { useNavigate } from "react-router-dom";

interface HeaderProps {
    version: string;
    initialized: boolean;
}

function HknHeader(props : HeaderProps) {
    const navigate = useNavigate();

    //const version = "V5.0.1";
    return (<>
        <div className="float-end">
            <p><a href="/kafkaflow" target="_blank">Kafka Flow Admin</a></p>
            <p><a href="https://github.com/leosperry/ha-kafka-net/wiki" target="_blank">Documentation</a></p>
        </div>
        <h1><span role="button" onClick={() => navigate('/')} ><img src={Logo}/> HaKafkaNet</span></h1>
        <div>Version:<span>{props.version}</span></div>
        <div>State Handler Initialized: <span>{props.initialized.toString()}</span></div>
        <hr />
    </>);
}

export default HknHeader;