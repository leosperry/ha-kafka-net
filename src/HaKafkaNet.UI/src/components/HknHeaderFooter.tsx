import Logo from '../assets/hkn_128.png';
import { useNavigate } from "react-router-dom";
import { useEffect, useState, PropsWithChildren, MouseEventHandler } from "react";
import { Api } from '../services/Api';
import { SystemInfo } from '../models/SystemInfo';
import icons from '../assets/icons/bootstrap-icons.svg';
import { Button, Card, CardBody, ListGroup, Offcanvas } from 'react-bootstrap';

function HknHeaderFooter(props: PropsWithChildren) {
  const navigate = useNavigate();

  const [data, setData] = useState<SystemInfo>();
  const [timerDisplay, setTimerDisplay] = useState<string | null>(null);
  const [showMenu, setShowMenu] = useState<boolean>(false);


  useEffect(() => {
    getData().then((b) => {
      setShowMenu(!b);
    });

  }, []);

  async function getData(): Promise<boolean> {
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
    return sysInfo.stateHandlerInitialized;
  }

  let ErrorLogClick: MouseEventHandler<Element> = event => {
    setShowMenu(false);
    navigate('log/error');
    event.preventDefault()
  };

  let GlobalLogClick: MouseEventHandler<Element> = event => {
    setShowMenu(false);
    navigate('log/global');
    event.preventDefault()
  };

  let TrackerLogClick: MouseEventHandler<Element> = event => {
    setShowMenu(false);
    navigate('log/tracker');
    event.preventDefault()
  };

  return !data ? (<></>) : (<>
    <div className='float-end mt-3'>
      <Button variant='primary' onClick={() => setShowMenu(!showMenu)} className='float-end'>
        <svg height={32} width={32} fill={"white"} fillOpacity={.5} >
          <use href={icons + "#list"} height={32} width={32} />
        </svg>
      </Button>

      <Offcanvas show={showMenu} onHide={() => setShowMenu(false)} backdrop={true} scroll={true} placement='end'>
        <Offcanvas.Header closeButton onClick={() => setShowMenu(false)}>
          <Offcanvas.Title>Resources</Offcanvas.Title>
        </Offcanvas.Header>
        <Offcanvas.Body>
          <p>
            State Handler Initialized: <span className='text-uppercase fw-bolder'>{data.stateHandlerInitialized.toString()}</span><br />{timerDisplay && <>{timerDisplay}</>}
          </p>
          <ListGroup>
            <ListGroup.Item variant="info" href="/hakafkanet/log/error" onClick={ErrorLogClick} action active={false}>Error Log</ListGroup.Item>
            <ListGroup.Item variant="info" href="/hakafkanet/log/tracker" onClick={TrackerLogClick} action active={false}>Entity Tracker Log</ListGroup.Item>
            <ListGroup.Item variant="info" href="/hakafkanet/log/global" onClick={GlobalLogClick} action active={false}>Global Log</ListGroup.Item>
            <ListGroup.Item variant="info" href="/kafkaflow" target="_blank" action active={false}>KafkaFlow Admin</ListGroup.Item>
            <ListGroup.Item variant="info" href="https://github.com/leosperry/ha-kafka-net/wiki" target='_blank' action active={false}>Documentation</ListGroup.Item>
          </ListGroup>
          <Card className='mt-5'>
            <CardBody>
              <Card.Title>Tip:</Card.Title>
              <Card.Text>
                You can manually trigger automations<br /> by setting entity state in <a href="http://homeassistant.local:8123/developer-tools/state" target="_blank">Home Assistant</a>
              </Card.Text>
            </CardBody>
          </Card>
        </Offcanvas.Body>
      </Offcanvas>
    </div>
    <div className='d-flex flex-row'>
      <div className='p-2'>
      <a href='/hakafkanet' onClick={e => { navigate('/'); e.preventDefault() }}><img src={Logo} /></a>
      </div>
      <div className='p-2 mt-3'>
        <h1 className=''>
          <a href='/hakafkanet' onClick={e => { navigate('/'); e.preventDefault() }} className='text-reset text-decoration-none'> HaKafkaNet</a>
        </h1>
        <div>Version: {data.version}</div>
      </div>
    </div>
    {/* begin page content */}
    {props.children}
    {/* end page content */}
    <div className='mt-3'>
      Send the developer a thank you, report a bug, or request feature via <a href={"mailto:leonard.sperry@live.com?subject=HaKafkaNet Comment V" + data.version}>email</a>
      &nbsp; or <a href="https://github.com/leosperry/ha-kafka-net/discussions" target="_blank">start a discussion</a>
    </div><br />
  </>);
}

export default HknHeaderFooter;