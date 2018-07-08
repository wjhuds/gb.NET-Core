import React from 'react';
import { Grid, Row, Col } from 'react-bootstrap';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';
import './DevConsole.css';

import SideNav from './Utilities/SideNav.js';
import FocusArea from './Utilities/FocusArea.js';
/* Displays */
import MainPanel from './Displays/MainPanel.js';
import TilemapPanel from './Displays/TilemapPanel.js';
import FullWindowPanel from './Displays/FullWindowPanel.js';
/* Memory */
import RegistersPanel from './Memory/RegistersPanel.js';
import StackPanel from './Memory/StackPanel.js';
import InstructionsPanel from './Memory/InstructionsPanel.js';
/* Output */
import ConsolePanel from './Output/ConsolePanel.js';
import TileContentPanel from './Output/TileContentPanel.js';

export default class DevConsole extends React.Component {
  constructor(props) {
    super(props);

    this.consoleMessages = [];

    this.displayTabs = [<MainPanel name='Main Display' />, <TilemapPanel name='Tilemap Display' />, <FullWindowPanel name='Full Window Display' />];
    this.memoryTabs = [<InstructionsPanel name='Instructions' />, <RegistersPanel name='Registers' />, <StackPanel name='Stack' />];
    this.outputTabs = [<ConsolePanel name='Console' contents={this.consoleMessages}/>, <TileContentPanel name='Tile Content' />];

    this.connection = new HubConnectionBuilder()
      .withUrl('http://localhost:56739/devhub')
      .configureLogging(LogLevel.Information)
      .build();

    this.connection.on('receiveConsoleLog', message => this.onConsoleLog(message));

    this.connection.start()
        .then(() => {
          this.connection.invoke('StartEmulation', ['testrom.gb', 'true'])
        });
    
    this.connection.connectionClosed(function() {
      this.connection.log('Connection closed. Retrying...');
      setTimeout(function() { this.connection.start(); }, 5000);
    });
  }
  
  onConsoleLog = (message) => {
    console.log('damn xd');
    this.consoleMessages.push(message);
  }

  render() {
    return (
      <div>
        <SideNav />
        
        <div className="mainPanel">
          <Grid fluid={true}>
            <Row>
              <Col md={8}>
                <FocusArea tabContent={this.displayTabs} divClass='displayArea' />
                <FocusArea tabContent={this.outputTabs} divClass='outputArea' />
              </Col>
              <Col md={4}>
                <FocusArea tabContent={this.memoryTabs} divClass='memoryArea' />
              </Col>
            </Row>
          </Grid>
        </div>
      </div>
    );
  }
}