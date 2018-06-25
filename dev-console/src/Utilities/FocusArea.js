import React from 'react';
import '../DevConsole.css';
import { TabButtonRow, TabContentRegion } from './Tabs.js';

export default class FocusArea extends React.Component {
  state = {
    activeTab: this.props.tabContent[0].props.name,
  }

  handleClick = (index) => this.setState({ activeTab: index });

  render() {
    return (
      <div class="displayArea">
        <TabButtonRow
          tabs={this.props.tabContent.map(t => t.props.name)}
          activeTab={this.state.activeTab}
          onClick={this.handleClick} />
        <TabContentRegion
          tabContent={this.props.tabContent}
          activeTab={this.state.activeTab} />
      </div>
    );
  }
}