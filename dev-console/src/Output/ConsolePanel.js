import React from 'react';

class ConsoleRow extends React.Component {
  render() {
    return (
      <li className="consoleRow">
        <div>
          {this.props.text}
        </div>
      </li>
    );
  }
}

export default class ConsolePanel extends React.Component {
  state = {
    contents: this.props.contents,
  }

  commponentWillReceiveProps(nextProps) {
    if(nextProps.contents !== this.props.contents){
      this.setState({contents: nextProps.contents});
    }
  }

  render() {
    const consoleEntries = this.state.contents.map(e => <ConsoleRow text={e} />);
    console.log(consoleEntries);
    return (
      <ul className="consoleList">
        {consoleEntries}
      </ul>
    );
  }
}