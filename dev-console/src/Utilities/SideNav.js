import React from 'react';
import Icon from './Icon.js';

export default class SideNav extends React.Component {
  render() {
    return (
      <div className="sidenav">
        <button>
          <Icon icon="reorder"/>
        </button>
        <button>
          <Icon icon="input"/>
        </button>
        <button>
          <Icon icon="speaker_notes"/>
        </button>
      </div>
    );
  }
}