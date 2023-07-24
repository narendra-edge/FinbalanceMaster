import './Index.css';
import Mutualfund from './components/Mutualfund';
import {useState} from 'react';


function App(){
     const [fundsHouse, setfundsHouse] = useState ();
     const showMutualfunds = true;
    return(       
          <div className='App'>
              {showMutualfunds ? (
               <>
                 <Mutualfund fundName= "HDFC"  fundsHouse="HDFC Asset management" />
                 <Mutualfund fundName= "ICICI"  fundsHouse={fundsHouse} />
               </>    
              ) : (
                 <p> You cannot see the funds</p>
           )}
           </div>  
          );
          
}
export default App;