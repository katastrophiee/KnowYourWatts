import React, { useEffect, useState } from "react";
import './App.css';
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap/dist/js/bootstrap.bundle.min.js';
import { MonitorData } from "./MonitorData";
import * as Styled from "./StyledComponents.tsx";
function App() {
    const [time, setTime] = useState<string>();
    const [homePageData, setHomePageData] = useState<MonitorData>();
    const [activeTab, setToggleState] = useState(1);

    useEffect(() => {

        setInterval(() => {
    
          const dateObject = new Date()
    
          const hour = dateObject.getHours()
          const minute = dateObject.getMinutes()
    
          const currentTime = hour + ':' + minute
          
          setTime(currentTime)
        }, 1000)
    
      }, [])

    return (
        <div className="App">
            <Styled.AppHeader>
                {time}
            </Styled.AppHeader>
            <div className="p-2">
                <Styled.NavTabs>
                    <Styled.NavItem>
                        <button className={activeTab === 1 ? "nav-link active" : "nav-link"} onClick={() => setToggleState(1)}>tab 1</button>
                    </Styled.NavItem>
                    <Styled.NavItem>
                        <button className={activeTab === 2 ? "nav-link active" : "nav-link"} onClick={() => setToggleState(2)}>tab 2</button>
                    </Styled.NavItem>
                    <Styled.NavItem>
                        <button className={activeTab === 3 ? "nav-link active" : "nav-link"} onClick={() => setToggleState(3)}>tab 3</button>
                    </Styled.NavItem>
                </Styled.NavTabs>

                <div className="content"> 
                    {activeTab === 1 && 
                        <Styled.TabPane>
                            <div className="container row">
                                <Styled.Usage>Usage:</Styled.Usage>
                                <div className="col-2 text-center"><svg className="icon"xmlns="http://www.w3.org/2000/svg" viewBox="0 0 384 512"><path fill="#FFD43B" d="M0 256L28.5 28c2-16 15.6-28 31.8-28H228.9c15 0 27.1 12.1 27.1 27.1c0 3.2-.6 6.5-1.7 9.5L208 160H347.3c20.2 0 36.7 16.4 36.7 36.7c0 7.4-2.2 14.6-6.4 20.7l-192.2 281c-5.9 8.6-15.6 13.7-25.9 13.7h-2.9c-15.7 0-28.5-12.8-28.5-28.5c0-2.3 .3-4.6 .9-6.9L176 288H32c-17.7 0-32-14.3-32-32z" /></svg></div>                            
                            </div>
                            <p className="px-3">&pound;1.80</p>
                            <p className="px-3">18KW</p>
                        </Styled.TabPane>}
                    {activeTab === 2 && 
                        <Styled.TabPane>
                            <div className="container row">
                                <Styled.Usage>Usage:</Styled.Usage>   
                                <div className="col-2 text-center"><svg className="icon"xmlns="http://www.w3.org/2000/svg" viewBox="0 0 384 512"><path fill="#FFD43B" d="M0 256L28.5 28c2-16 15.6-28 31.8-28H228.9c15 0 27.1 12.1 27.1 27.1c0 3.2-.6 6.5-1.7 9.5L208 160H347.3c20.2 0 36.7 16.4 36.7 36.7c0 7.4-2.2 14.6-6.4 20.7l-192.2 281c-5.9 8.6-15.6 13.7-25.9 13.7h-2.9c-15.7 0-28.5-12.8-28.5-28.5c0-2.3 .3-4.6 .9-6.9L176 288H32c-17.7 0-32-14.3-32-32z" /></svg></div>
                            </div>
                            <p className="px-3">&pound;1.80</p>
                            <p className="px-3">18KW</p>
                        </Styled.TabPane>}
                    {activeTab === 3 && 
                        <Styled.TabPane>
                            <div className="container row">
                                <Styled.Usage>Usage:</Styled.Usage>
                                <div className="col-2">⚡</div>
                            </div>
                            <p className="px-3">&pound;1.80</p>
                            <p className="px-3">18KW</p>
                        </Styled.TabPane>}
                </div>
            </div>
        </div>
          
    );
}

export default App;

