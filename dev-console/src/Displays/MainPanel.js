import React from 'react';

export default class MainDisplay extends React.Component {  
  render() {
    return <div>{this.props.name}</div>;
  }
}