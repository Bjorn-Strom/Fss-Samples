import React, {useState} from 'react';
import {buttonStyle, container, header, inputStyle, todoStyle} from "./Styles/js/Library";

export const App = () => {
    const [input, setInput] = useState("")
    const [todos, setTodos] = useState<string[]>([])

    return (
        <div className={container}>
            <h2 className={header}>TODO</h2>
            <ul>
                {todos.map(t => <li className={todoStyle}>{t}</li>)}
            </ul>
            <div>
                <input
                    className={inputStyle}
                    onChange={e => setInput(e.target.value)}
                    value={input}
                    placeholder="What needs to be done?"/>
                <button
                    className={buttonStyle}
                    onClick={() => {
                        setTodos([...todos, input])
                        setInput("")
                    }}>
                    Add #{todos.length}
                </button>
            </div>
        </div>
    );
}
