'use strict';

const e = React.createElement;

class HaKafkaNetRoot extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            liked: false //throw away
        };
    }

    render() {
        return (
            <div>
                <h1>Hello HaKafkaNetUser</h1>
                <button type="button">click me</button>
            </div>
        )
        //if (this.state.liked) {
        //    return 'You liked this.';
        //}

        //return e(
        //    'button',
        //    { onClick: () => this.setState({ liked: true }) },
        //    'Like'
        //);
    }
}

const domContainer = document.querySelector('#hakafkanet');
const root = ReactDOM.createRoot(domContainer);
root.render(e(HaKafkaNetRoot));