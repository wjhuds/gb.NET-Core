import React from 'react';
import { Grid, Row, Col } from 'react-bootstrap';
import './DevConsole.css';

import FocusArea from './Utilities/FocusArea.js';
import Icon from './Utilities/Icon.js';
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

const displayTabs = [<MainPanel name='Main Display' />, <TilemapPanel name='Tilemap Display' />, <FullWindowPanel name='Full Window Display' />];
const memoryTabs = [<InstructionsPanel name='Instructions' />, <RegistersPanel name='Registers' />, <StackPanel name='Stack' />];
const outputTabs = [<ConsolePanel name='Console' />, <TileContentPanel name='Tile Content' />];

export default class DevConsole extends React.Component {
  render() {
    return (
      <div>
        {/* Roll this into its own component later */}
        <div className="sidenav">
          <a href="#">
            <Icon icon="reorder"/>
          </a>
          <a href="#">
            <Icon icon="input"/>
          </a>
          <a href="#">
            <Icon icon="speaker_notes"/>
          </a>
        </div>

        <div className="mainPanel">
          <Grid fluid={true}>
            <Row>
              <Col md={8}>
                <FocusArea tabContent={displayTabs} className='displayArea' />
                <FocusArea tabContent={outputTabs} className='outputArea' />
              </Col>
              <Col md={4}>
                <FocusArea tabContent={memoryTabs} className='memoryArea' />
              </Col>
            </Row>
          </Grid>
        </div>
      </div>
    );
  }
}