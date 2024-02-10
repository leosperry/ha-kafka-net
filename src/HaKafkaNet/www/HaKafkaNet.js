const e = React.createElement;

class HaKafkaNetRoot extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            systemInfo: {},
            error:null
        };
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
    }

    render() {
        return (
            <div>
                <div className="float-end">
                    <p><a href="/kafkaflow" target="_blank">Kafka Flow Admin</a></p>
                    <p><a href="https://github.com/leosperry/ha-kafka-net/wiki" target="_blank">Documentation</a></p>
                </div>
                <h1>Ha-Kafka-Net</h1>
                <div>Version {this.state.systemInfo.data &&
                    <span>{this.state.systemInfo.data.version}</span>
                }</div>
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
                <div>
                    <p>Tip: you can trigger your automations manually by setting entity state in <a href="http://homeassistant.local:8123/developer-tools/state" target="_blank">Home Assistant</a></p>
                </div>
                <table className="table table-bordered table-hover">
                    <thead>
                        <tr>
                            <th>Enabled</th>
                            <th>Name</th>
                            <th>Description</th>
                            <th>Type</th>
                            <th>Source</th>
                            <th>Entity IDs</th>
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
                                    <td>{item.source}</td>
                                    <td>

                                    <div id="accordion">
                                        <div className="card">
                                            <div className="card-header" id={"heading1" + item.id}>
                                                <h5 className="mb-0">
                                                    <button className="btn btn-link collapsed" data-bs-toggle="collapse" data-bs-target={"#triggers" + item.id} aria-expanded="false" aria-controls={"triggers" + item.id}>
                                                    Triggers
                                                    </button>
                                                </h5>
                                            </div>  
                                            <div id={"triggers" + item.id} className="collapse" aria-labelledby={"heading1" + item.id} data-parent="#accordion">
                                                <div className="card-body">
                                                    {item.triggerIds.map(trigger =>(<p key={trigger}>{trigger}</p>))}
                                                </div>
                                            </div>
                                        </div>
                                        <div className="card">
                                            <div className="card-header" id={"heading2" + item.id}>
                                                <h5 className="mb-0">
                                                    <button className="btn btn-link collapsed" data-bs-toggle="collapse" data-bs-target={"#additional" + item.id} aria-expanded="false" aria-controls="collapseOne">
                                                    Additional Entities
                                                    </button>
                                                </h5>
                                            </div>  
                                            <div id={"additional" + item.id} className="collapse" aria-labelledby={"heading2" + item.id} data-parent="#accordion">
                                                <div className="card-body">
                                                    {item.additionalEntitiesToTrack.map(e =>(<p key={e}>{e} </p>))}
                                                </div>
                                            </div>
                                        </div>                                        
                                    </div>
                                    </td>
                                </tr>
                            ))
                        }
                    </tbody>
                </table>

                {this.state.systemInfo.data && 
                    <div>
                        Send the developer a thank you, report a bug, or request feature via <a href={"mailto:leonard.sperry@live.com?subject=HaKafkaNet Comment V"+ this.state.systemInfo.data.version}>email</a> 
                        &nbsp; or <a href="https://github.com/leosperry/ha-kafka-net/discussions" target="_blank">start a discussion</a>
                    </div>
                }
            </div>
        )
    }
}

const domContainer = document.querySelector('#hakafkanet');
const root = ReactDOM.createRoot(domContainer);
root.render(e(HaKafkaNetRoot));