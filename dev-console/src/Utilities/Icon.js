import React from 'react';
import '../DevConsole.css';

export default class Icon extends React.Component {
  render() {
    const icon = this.props.icon; 

    return (
      <i className={'material-icons navIcon'}>{icon}</i>
    )
  }
}