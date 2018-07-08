import React from 'react';
import classnames from 'classnames';
import '../DevConsole.css';

class TabButton extends React.Component {
  render() {
    return (
      <li className="horizontalRowItem">
        <button
          className={classnames('tabButton', {selected: this.props.isSelected})}
          onClick={() => {this.props.onClick(this.props.name)}}>
            {this.props.name}
        </button>
      </li>
    )
  }
}

export class TabButtonRow extends React.Component {
  render() {
    const buttons = this.props.tabs.map((tab) => 
      <TabButton
        key={tab}
        name={tab}
        isSelected={this.props.activeTab === tab}
        onClick={this.props.onClick} />
    );

    return (
      <ul className="tabButtonRow" >{buttons}</ul>
    );
  }
}

export class TabContentRegion extends React.Component {
  render() {
    return (
      <div className="tabContentRegion">
        {this.props.tabContent.find(t => t && (t.props.name === this.props.activeTab))}
      </div>
    );
  }
}