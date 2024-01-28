const e = React.createElement;

class HaKafkaNetRoot extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            systemInfo: {},
            error:null
        };
        //this.onToggleLoop = this.onToggleLoop.bind(this);
        this.handleCheckboxChange = this.handleCheckboxChange.bind(this);
    }

    componentDidMount(){
        let sysInfoUrl = '/api/systeminfo'
        fetch(sysInfoUrl)
        .then(response => response.json())
        .then(sysInfo =>{
            this.setState({systemInfo : sysInfo})
        })
        .catch(error =>{
            this.setState({error})
        })
    }

    async handleCheckboxChange(evt)
    {
        var automationId = evt.target.getAttribute('data-id');
        var checked = evt.target.checked;

        var payload = {
            id : automationId,
            enable: checked
        };

        const response = await fetch('../api/automation/enable', {
            method: 'POST',
            headers:{
                "Content-Type": "application/json"
            },
            body: JSON.stringify(payload)
        });

        if (response.ok) {
           this.setState(prevState => {
            prevState.systemInfo.data.automations[automationId].enabled = checked;
            return {systemInfo: prevState.systemInfo};
           });
        }
        else{
            
        }


    }

    render() {
        return (
            <div>
                <h1>Ha-Kafka-Net</h1>
                {this.state.error &&
                    <h3 className="bg-warning">{this.state.error}</h3>
                }
                <div>State Handler Initialized:
                    {this.state.systemInfo.data &&
                        <mark className="text-uppercase">{this.state.systemInfo.data.stateHandlerInitialized.toString()}</mark>
                    }
                </div>
                <hr />
                <h3>Automations</h3>
                <table className="table table-bordered table-hover">
                    <thead>
                        <tr>
                            <th className="col-1 text-wrap">Enabled</th>
                            <th className="col-2 text-wrap">Name</th>
                            <th className="col-1 text-wrap">Description</th>
                            <th className="col-1 text-wrap">Type</th>
                            <th className="col-1 text-wrap">Trigger IDs</th>
                        </tr>
                    </thead>
                    <tbody>
                        { this.state.systemInfo.data && 
                            Object.entries(this.state.systemInfo.data.automations).map(([id, item]) =>(
                                <tr key={item.id.toString()}>
                                    <td>
                                        <div className="form-check form-switch">
                                            <input className="form-check-input" type="checkbox" checked={item.enabled ? 'checked' : ''} onChange={this.handleCheckboxChange} data-id={item.id} />
                                        </div>
                                    </td>
                                    <td>{item.name}</td>
                                    <td>{item.description}</td>
                                    <td>{item.typeName}</td>
                                    <td>{item.triggerIds}</td>
                                </tr>
                            ))
                        }
                    </tbody>
                </table>
            </div>
        )
    }
}

const domContainer = document.querySelector('#hakafkanet');
const root = ReactDOM.createRoot(domContainer);
root.render(e(HaKafkaNetRoot));