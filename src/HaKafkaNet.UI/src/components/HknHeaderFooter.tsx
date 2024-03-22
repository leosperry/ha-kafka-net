import Logo from '../assets/hkn_128.png';
import { useNavigate } from "react-router-dom";
import {useEffect, useState, PropsWithChildren  } from "react";
import { Api } from '../services/Api';
import { SystemInfo } from '../models/SystemInfo';

function HknHeaderFooter(props: PropsWithChildren) {
    const navigate = useNavigate();

    const [data, setData] = useState<SystemInfo>();
    const [timerDisplay, setTimerDisplay] = useState<string | null>(null);


    useEffect(() => {
      getData();
    }, []);
  
    async function getData() {
      setTimerDisplay("Checking ...");

      var sysInfo = await Api.GetSystemInfo();
      setData(sysInfo);

      if (sysInfo.stateHandlerInitialized) {
        setTimerDisplay(null);
      } else {
        setTimerDisplay("Checking again in 5");
        let secondCount = 5;
        let timerID = setInterval(() => {
          secondCount--;
          setTimerDisplay("Checking again in " + secondCount.toString());
          if (secondCount == 0) {
            clearInterval(timerID);
            getData();
          }
        }, 1000)
      }
    }
  
    return !data ? (<></>) : (<>
        <div className="float-end">
            <br />
            <p><a href="/kafkaflow" target="_blank">Kafka Flow Admin</a></p>
            <p><a href="https://github.com/leosperry/ha-kafka-net/wiki" target="_blank">Documentation</a></p>
        </div>
        <h1><span role="button" onClick={() => navigate('/')} ><img src={Logo}/> HaKafkaNet</span></h1>
        <div>Version:<span>{data.version}</span></div>
        <div>
          State Handler Initialized: <span className='text-uppercase fw-bolder'>{data.stateHandlerInitialized.toString()}</span>&nbsp;{timerDisplay && <>{timerDisplay}</>}
        </div>
        <hr />
        {props.children}
        <div className='mt-3'>
          Send the developer a thank you, report a bug, or request feature via <a href={"mailto:leonard.sperry@live.com?subject=HaKafkaNet Comment V"+ data.version}>email</a> 
          &nbsp; or <a href="https://github.com/leosperry/ha-kafka-net/discussions" target="_blank">start a discussion</a>
      </div><br />
</>);
}

export default HknHeaderFooter;