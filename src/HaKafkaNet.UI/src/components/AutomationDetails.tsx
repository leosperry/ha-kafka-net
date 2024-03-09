import { useParams } from 'react-router-dom';


function AutomationDetails() {
    const {key} = useParams();
    return (<>
        <h3>Automation Details</h3>
        <div>{key}</div>
    </>);
}

export default AutomationDetails;