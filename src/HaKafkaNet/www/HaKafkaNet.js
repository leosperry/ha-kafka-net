'use strict';

const e = React.createElement;

class HaKafkaNetRoot extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            systemInfo: {},
            error:null
        };
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

    render() {
        return (
            <div>
                <h1>Ha-Kafka-Net</h1>
                {this.state.error &&
                    <h3 class="bg-warning">{this.state.error}</h3>
                }
                <div>State Handler Initialized:
                    {this.state.systemInfo.data &&
                        <mark class="text-uppercase">{this.state.systemInfo.data.stateHandlerInitialized.toString()}</mark>
                    }
                </div>
                <hr />
                <h3>Automations</h3>
                <table class="table table-bordered table-hover">
                    <thead>
                        <tr>
                            {/* <th class="col-xs-1">Enabled</th> */}
                            <th class="col-xs-2">Name</th>
                            <th class="col-xs-1">Type</th>
                            <th class="col-xs-1">Trigger IDs</th>
                        </tr>
                    </thead>
                    <tbody>
                        { this.state.systemInfo.data && 
                            this.state.systemInfo.data.automations.map((item) =>(
                                <tr>
                                    {/* <td>
                                        <div class="form-check form-switch">
                                            <input class="form-check-input" type="checkbox" />
                                        </div>
                                    </td> */}
                                    <td>{item.name}</td>
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